import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router'; 

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent {
  loginForm: FormGroup;

  constructor(
    private fb: FormBuilder,
    private http: HttpClient,
    private router: Router
  ) {
    this.loginForm = this.fb.group({
      Email: ['', [Validators.required, Validators.email]],
      Password: ['', Validators.required]
    });
  }

  onSubmit() {
    if (this.loginForm.valid) {
      const loginData = {
        Email: this.loginForm.get('Email')?.value,
        Password: this.loginForm.get('Password')?.value,
      };
      console.log('Dados enviados:', loginData);
      this.http.post<any>('https://localhost:7186/api/auth/login', loginData).subscribe({
        next: (res) => {
          console.log('Login response:', res);
          localStorage.setItem('token', res.token);
          localStorage.setItem('nomeUtilizador', res.nome);
          this.router.navigate(['/dashboard']);
        },
        error: (err) => {
          console.error('Erro no login:', err);
          alert('Credenciais inválidas');
        }
      });
    }
  }
}
