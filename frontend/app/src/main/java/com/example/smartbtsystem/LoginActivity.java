package com.example.smartbtsystem;

import android.content.Intent;
import android.content.SharedPreferences;
import android.os.Bundle;
import android.widget.TextView;
import android.widget.Toast;
import androidx.appcompat.app.AppCompatActivity;
import com.example.smartbtsystem.network.ApiClient;
import com.example.smartbtsystem.network.ApiService;
import com.google.android.material.button.MaterialButton;
import com.google.android.material.textfield.TextInputEditText;
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

public class LoginActivity extends AppCompatActivity {

    private TextInputEditText etEmail, etPassword;
    private MaterialButton btnLogin;
    private TextView tvSignup;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_login);

        etEmail = findViewById(R.id.et_email);
        etPassword = findViewById(R.id.et_password);
        btnLogin = findViewById(R.id.btn_login);
        tvSignup = findViewById(R.id.tv_signup);

        btnLogin.setOnClickListener(v -> {
            String email = etEmail.getText().toString();
            String password = etPassword.getText().toString();

            if (email.isEmpty() || password.isEmpty()) {
                Toast.makeText(this, "Please fill all fields", Toast.LENGTH_SHORT).show();
            } else {
                performLogin(email, password);
            }
        });

        tvSignup.setOnClickListener(v -> startActivity(new Intent(this, SignupActivity.class)));
    }

    private void performLogin(String email, String password) {
        ApiService.LoginRequest request = new ApiService.LoginRequest(email, password);

        ApiClient.getService().login(request).enqueue(new Callback<ApiService.ApiResponse<ApiService.AuthResponse>>() {
            @Override
            public void onResponse(Call<ApiService.ApiResponse<ApiService.AuthResponse>> call,
                                   Response<ApiService.ApiResponse<ApiService.AuthResponse>> response) {

                // 1. Check if the HTTP request worked (200 OK)
                if (response.isSuccessful() && response.body() != null) {
                    ApiService.ApiResponse<ApiService.AuthResponse> apiResponse = response.body();

                    // 2. Check if the Backend logic said "Success"
                    if (apiResponse.success) {
                        String token = apiResponse.data.token; // Access data inside the wrapper

                        SharedPreferences pref = getSharedPreferences("AUTH", MODE_PRIVATE);
                        pref.edit().putString("token", token).apply();

                        startActivity(new Intent(LoginActivity.this, MainActivity.class));
                        finish();
                    } else {
                        Toast.makeText(LoginActivity.this, apiResponse.message, Toast.LENGTH_SHORT).show();
                    }
                }
            }

            @Override
            public void onFailure(Call<ApiService.ApiResponse<ApiService.AuthResponse>> call, Throwable t) {
                Toast.makeText(LoginActivity.this, "Network Error", Toast.LENGTH_SHORT).show();
            }
        });
    }
}