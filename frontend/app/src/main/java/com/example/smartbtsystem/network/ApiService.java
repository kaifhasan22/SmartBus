package com.example.smartbtsystem.network;

import com.google.gson.annotations.SerializedName;
import java.util.List;
import retrofit2.Call;
import retrofit2.http.Body;
import retrofit2.http.GET;
import retrofit2.http.POST;
import retrofit2.http.Path;

public interface ApiService {

    // Matches [HttpPost("login")] in AuthController.cs
    @POST("auth/login")
    Call<ApiResponse<AuthResponse>> login(@Body LoginRequest request);

    // Matches [HttpGet("{routeName}/location")] in BusController.cs
    @GET("buses/{routeName}/location")
    Call<ApiResponse<BusLocationResponse>> getBusLocation(@Path("routeName") String routeName);

    // --- The Wrapper (Crucial!) ---
    class ApiResponse<T> {
        @SerializedName("Success") public boolean success;
        @SerializedName("Message") public String message;
        @SerializedName("Data") public T data;
    }

    // --- Data Blueprints (Models) ---
    class LoginRequest {
        public String Email, Password;
        public LoginRequest(String e, String p) { this.Email = e; this.Password = p; }
    }

    class AuthResponse {
        @SerializedName("Token") public String token;
        @SerializedName("Role") public String role;
        @SerializedName("Name") public String name;
    }

    class BusLocationResponse {
        @SerializedName("Latitude") public double latitude;
        @SerializedName("Longitude") public double longitude;
        @SerializedName("Distance") public double distance;
        @SerializedName("Eta") public int eta;
    }
}