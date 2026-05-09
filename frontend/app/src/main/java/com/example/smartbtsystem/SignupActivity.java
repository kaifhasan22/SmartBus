package com.example.smartbtsystem;

import android.content.Intent;
import android.os.Bundle;
import android.widget.TextView;
import android.widget.Toast;

import androidx.appcompat.app.AppCompatActivity;

import com.google.android.material.button.MaterialButton;
import com.google.android.material.textfield.TextInputEditText;

public class SignupActivity extends AppCompatActivity {

    private TextInputEditText etName, etEmail, etPassword;
    private MaterialButton btnSignup;
    private TextView tvLogin;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_signup);

        etName = findViewById(R.id.et_name);
        etEmail = findViewById(R.id.et_email_signup);
        etPassword = findViewById(R.id.et_password_signup);
        btnSignup = findViewById(R.id.btn_signup);
        tvLogin = findViewById(R.id.tv_login);

        btnSignup.setOnClickListener(v -> {
            String name = etName.getText().toString();
            String email = etEmail.getText().toString();
            String password = etPassword.getText().toString();

            if (name.isEmpty() || email.isEmpty() || password.isEmpty()) {
                Toast.makeText(this, "Please fill all fields", Toast.LENGTH_SHORT).show();
            } else {
                Toast.makeText(this, "Account Created Successfully", Toast.LENGTH_SHORT).show();
                Intent intent = new Intent(SignupActivity.this, LoginActivity.class);
                startActivity(intent);
                finish();
            }
        });

        tvLogin.setOnClickListener(v -> {
            finish(); // Go back to Login
        });
    }
}
