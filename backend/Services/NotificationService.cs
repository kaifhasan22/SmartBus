using Microsoft.EntityFrameworkCore;
using SmartBusAPI.Data;
using SmartBusAPI.DTOs;
using SmartBusAPI.Models;

namespace SmartBusAPI.Services
{
    public interface INotificationService
    {
        Task<List<NotificationDto>> GetForUserAsync(int userId);
        Task                        SendAsync(int userId, string title, string message, string type = "General");
        Task                        BroadcastAsync(string role, string title, string message, string type = "General");
        Task<bool>                  MarkReadAsync(int notificationId, int userId);
        Task<int>                   MarkAllReadAsync(int userId);
    }

    public class NotificationService : INotificationService
    {
        private readonly AppDbContext _db;

        public NotificationService(AppDbContext db) => _db = db;

        public async Task<List<NotificationDto>> GetForUserAsync(int userId)
        {
            return await _db.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.SentAt)
                .Take(50)
                .Select(n => new NotificationDto
                {
                    Id      = n.Id,
                    Title   = n.Title,
                    Message = n.Message,
                    Type    = n.Type,
                    IsRead  = n.IsRead,
                    SentAt  = n.SentAt
                })
                .ToListAsync();
        }

        public async Task SendAsync(int userId, string title, string message, string type = "General")
        {
            var notif = new Notification
            {
                UserId  = userId,
                Title   = title,
                Message = message,
                Type    = type,
                IsRead  = false,
                SentAt  = DateTime.UtcNow
            };

            _db.Notifications.Add(notif);
            await _db.SaveChangesAsync();

            // Send FCM push to user's device
            var user = await _db.Users.FindAsync(userId);
            if (!string.IsNullOrEmpty(user?.FCMToken))
                await SendFcmAsync(user.FCMToken, title, message);
        }

        public async Task BroadcastAsync(string role, string title, string message, string type = "General")
        {
            var users = await _db.Users
                .Where(u => u.Role == role && u.IsActive)
                .ToListAsync();

            var notifications = users.Select(u => new Notification
            {
                UserId  = u.Id,
                Title   = title,
                Message = message,
                Type    = type,
                IsRead  = false,
                SentAt  = DateTime.UtcNow
            }).ToList();

            _db.Notifications.AddRange(notifications);
            await _db.SaveChangesAsync();

            // FCM push to each user with a token
            foreach (var user in users.Where(u => !string.IsNullOrEmpty(u.FCMToken)))
                await SendFcmAsync(user.FCMToken!, title, message);
        }

        public async Task<bool> MarkReadAsync(int notificationId, int userId)
        {
            var n = await _db.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);
            if (n == null) return false;
            n.IsRead = true;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<int> MarkAllReadAsync(int userId)
        {
            var unread = await _db.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();
            unread.ForEach(n => n.IsRead = true);
            await _db.SaveChangesAsync();
            return unread.Count;
        }

        // FCM push notification via Firebase Admin SDK
        private static async Task SendFcmAsync(string fcmToken, string title, string body)
        {
            try
            {
                // Firebase Admin SDK call
                // Uncomment when you have firebase-adminsdk.json configured:
                //
                // var message = new FirebaseAdmin.Messaging.Message
                // {
                //     Token = fcmToken,
                //     Notification = new FirebaseAdmin.Messaging.Notification
                //     {
                //         Title = title,
                //         Body  = body
                //     }
                // };
                // await FirebaseAdmin.Messaging.FirebaseMessaging.DefaultInstance.SendAsync(message);

                await Task.CompletedTask; // placeholder until Firebase is configured
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FCM send failed: {ex.Message}");
            }
        }
    }
}
