package com.example.smartbtsystem;

import android.os.Bundle;
import android.os.Handler;
import android.widget.TextView;
import android.widget.Toast;
import androidx.appcompat.app.AppCompatActivity;
import com.example.smartbtsystem.network.ApiClient;
import com.example.smartbtsystem.network.ApiService;
import com.google.android.material.appbar.MaterialToolbar;
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class TrackingActivity extends AppCompatActivity {

    private TextView tvBusName, tvDestination, tvDistance, tvEta;
    private Handler handler = new Handler();
    private Runnable apiPollingRunnable;
    private String routeName;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_tracking);

        tvBusName = findViewById(R.id.tv_bus_name);
        tvDestination = findViewById(R.id.tv_destination);
        tvDistance = findViewById(R.id.tv_distance);
        tvEta = findViewById(R.id.tv_eta);

        routeName = getIntent().getStringExtra("ROUTE_NAME");
        String stop = getIntent().getStringExtra("STOP_NAME");

        tvBusName.setText(routeName);
        tvDestination.setText("Approaching " + stop);

        startTracking();
    }

    private void startTracking() {
        apiPollingRunnable = new Runnable() {
            @Override
            public void run() {
                fetchBusLocation();
                handler.postDelayed(this, 5000); // Update every 5 seconds
            }
        };
        handler.post(apiPollingRunnable);
    }

    private void fetchBusLocation() {
        ApiClient.getService().getBusLocation(routeName).enqueue(new Callback<ApiService.ApiResponse<ApiService.BusLocationResponse>>() {
            @Override
            public void onResponse(Call<ApiService.ApiResponse<ApiService.BusLocationResponse>> call, Response<ApiService.ApiResponse<ApiService.BusLocationResponse>> response) {
                if (response.isSuccessful() && response.body() != null && response.body().success) {
                    ApiService.BusLocationResponse data = response.body().data;
                    tvDistance.setText(String.format("%.1f km", data.distance));
                    tvEta.setText(data.eta + " mins");
                }
            }

            @Override
            public void onFailure(Call<ApiService.ApiResponse<ApiService.BusLocationResponse>> call, Throwable t) {
                // Silently fail to keep trying next time
            }
        });
    }

    @Override
    protected void onDestroy() {
        super.onDestroy();
        handler.removeCallbacks(apiPollingRunnable);
    }
}