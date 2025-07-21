import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
 
@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './register.component.html',
  styleUrl: './register.component.scss'
})
export class RegisterComponent {
  registerForm: FormGroup;
 
  constructor(
    private fb: FormBuilder,
    private http: HttpClient,
    private router: Router
  ) {
    this.registerForm = this.fb.group({
      Nome: ['', Validators.required],
      Email: ['', [Validators.required, Validators.email]],
      Password: ['', [Validators.required, Validators.minLength(6)]],
      NIF: ['', Validators.required],
      CartaoCidadao: ['', Validators.required],
      Telemovel: [''],
      DataNascimento: [''],
      Profissao: [''],
      Morada: ['']
    });
  }
 
  onSubmit() {
    console.log('Enviando formulário:', this.registerForm.value);
    if (this.registerForm.valid) {
      this.http.post<any>('https://localhost:7186/api/auth/register', this.registerForm.value).subscribe({
        next: () => {
          alert('Registo efetuado com sucesso');
          this.router.navigate(['/login']);
        },
            error: (err) => {
        console.error('Erro de registo:', err); 
        alert('Erro ao registar utilizador');
      }
      });
    }
  }
 
}
 
 