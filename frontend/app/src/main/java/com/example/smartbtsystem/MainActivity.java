package com.example.smartbtsystem;

import android.content.Intent;
import android.os.Bundle;
import android.widget.ArrayAdapter;
import android.widget.AutoCompleteTextView;
import android.widget.ImageView;
import android.widget.LinearLayout;
import android.widget.Toast;

import androidx.appcompat.app.AppCompatActivity;

import com.google.android.material.button.MaterialButton;

public class MainActivity extends AppCompatActivity {

    private AutoCompleteTextView routeSpinner;
    private AutoCompleteTextView stopSpinner;
    private MaterialButton btnTrack;
    private ImageView profileImage;
    private LinearLayout btnHistory, btnFavorites, btnSchedule;

    private String[] routes = {
            "Route 101: City Center - Airport",
            "Route 102: Downtown - Harbor",
            "Route 202: Railway Station - Tech Park",
            "Route 205: West End - Education Hub",
            "Route 303: North Mall - South Beach",
            "Route 310: Suburban - Business District",
            "Route 401: Hospital - Green Valley",
            "Route 502: Stadium - Metro Plaza"
    };

    private String[] stops = {
            "Central Square", "Green Park", "Hospital Junction", "High School",
            "University Gate", "Harbor Front", "Tech Park Main", "Shopping Mall",
            "Railway Station", "Beach Side", "Metro Station", "Stadium East"
    };

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        routeSpinner = findViewById(R.id.route_spinner);
        stopSpinner = findViewById(R.id.stop_spinner);
        btnTrack = findViewById(R.id.btn_track);
        profileImage = findViewById(R.id.profile_image);
        btnHistory = findViewById(R.id.btn_history);
        btnFavorites = findViewById(R.id.btn_favorites);
        btnSchedule = findViewById(R.id.btn_schedule);

        // Setup Adapters for Dropdowns
        ArrayAdapter<String> routeAdapter = new ArrayAdapter<>(this, android.R.layout.simple_list_item_1, routes);
        routeSpinner.setAdapter(routeAdapter);

        ArrayAdapter<String> stopAdapter = new ArrayAdapter<>(this, android.R.layout.simple_list_item_1, stops);
        stopSpinner.setAdapter(stopAdapter);

        // Profile Click
        profileImage.setOnClickListener(v -> {
            Intent intent = new Intent(MainActivity.this, ProfileActivity.class);
            startActivity(intent);
        });

        // Quick Action Clicks
        btnHistory.setOnClickListener(v -> Toast.makeText(this, "Opening Trip History...", Toast.LENGTH_SHORT).show());
        btnFavorites.setOnClickListener(v -> Toast.makeText(this, "Opening Favorite Routes...", Toast.LENGTH_SHORT).show());
        btnSchedule.setOnClickListener(v -> Toast.makeText(this, "Opening Bus Schedules...", Toast.LENGTH_SHORT).show());

        btnTrack.setOnClickListener(v -> {
            String selectedRoute = routeSpinner.getText().toString();
            String selectedStop = stopSpinner.getText().toString();

            if (selectedRoute.isEmpty() || selectedStop.isEmpty()) {
                Toast.makeText(MainActivity.this, "Please select both route and stop", Toast.LENGTH_SHORT).show();
            } else {
                Intent intent = new Intent(MainActivity.this, TrackingActivity.class);
                intent.putExtra("ROUTE_NAME", selectedRoute);
                intent.putExtra("STOP_NAME", selectedStop);
                startActivity(intent);
            }
        });
    }
}
