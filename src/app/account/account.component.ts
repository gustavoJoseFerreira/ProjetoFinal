import { CommonModule } from '@angular/common';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';

@Component({
  selector: 'app-account',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './account.component.html',
  styleUrl: './account.component.scss'
})
export class AccountComponent implements OnInit {
  pagamentoForm: FormGroup;
  mensagemErro: string = '';
  mensagemSucesso: string = '';
  contas: any[] = [];
  contaSelecionada: any = null;
  movimentos: any[] = [];
  

  private baseApiUrl = 'https://localhost:7186/api/Pagamento';
  historico: any[] = [];
  pagamentos: any;
  nomeUtilizador: string = '';

  constructor(private fb: FormBuilder, private http: HttpClient, private router: Router) {
    this.pagamentoForm = this.fb.group({
      ibanOrigem: ['', Validators.required],  
      entidade: ['', [Validators.required, Validators.maxLength(150)]],
      referencia: ['', [Validators.required, Validators.maxLength(50)]],
      valor: [null, [Validators.required, Validators.min(0.01)]],
    });
    
  }

  ngOnInit(): void {
    const nome = localStorage.getItem('nomeUtilizador');
    this.nomeUtilizador = nome ?? 'Utilizador';
    this.carregarContas();
    this.carregarHistorico();
  }

  getAuthHeaders(): HttpHeaders {
    const token = localStorage.getItem('token');
    if (token) {
      return new HttpHeaders({
        'Authorization': `Bearer ${token}`
      });
    }
    return new HttpHeaders();
  }

  carregarContas() {
    const headers = this.getAuthHeaders();

    this.http.get<any[]>(`https://localhost:7186/api/ContaBancaria/minhascontas`, { headers }).subscribe({
      next: (contas) => {
        this.contas = contas;
        if (contas.length > 0) {
          this.contaSelecionada = contas[0];
          this.pagamentoForm.patchValue({ ibanOrigem: this.contaSelecionada.iban });
          this.mensagemErro = '';
        } else {
          this.mensagemErro = 'Não existem contas bancárias associadas.';
          this.movimentos = [];
          this.contaSelecionada = null;
        }
      },
      error: (err) => {
        console.error('Erro ao carregar contas', err);
        if (err.status === 401 || err.status === 403) {
          this.router.navigate(['/login']);
        } else {
          this.mensagemErro = 'Erro ao carregar contas. Tente novamente mais tarde.';
        }
      }
    });
  }

  enviarPagamento() {
    if (this.pagamentoForm.invalid) {
      this.mensagemErro = 'Por favor, preencha o formulário corretamente.';
      return;
    }
  
    const headers = this.getAuthHeaders();
  
    
    const pagamentoDto = {
      IBAN: this.pagamentoForm.value.ibanOrigem,
      Entidade: this.pagamentoForm.value.entidade,
      Referencia: this.pagamentoForm.value.referencia,
      Valor: this.pagamentoForm.value.valor
    };
    console.log('Pagamento a enviar (JSON):', JSON.stringify(pagamentoDto, null, 2));
    this.http.post(this.baseApiUrl, pagamentoDto, { headers }).subscribe({
      next: () => {
        alert('Pagamento realizada com sucesso!');        
        this.pagamentoForm.reset();
        this.carregarHistorico();
      },
      error: (error) => {
        console.error('Erro ao realizar pagamento:', error);
  
        
        if (error.error && error.error.errors) {
          for (const campo in error.error.errors) {
            if (error.error.errors.hasOwnProperty(campo)) {
              console.error(`${campo}: ${error.error.errors[campo].join(', ')}`);
            }
          }
        }
  
        this.mensagemErro = 'Erro ao realizar pagamento. Verifique os dados e tente novamente.';
        this.mensagemSucesso = '';
      }
    });
  }

  carregarHistorico() {
    this.http.get<any[]>(`${this.baseApiUrl}/meuspagamentos`, {
      headers: this.getAuthHeaders()
    }).subscribe({
      next: (data) => this.historico = data,
      error: (err) => {
        console.error('Erro ao carregar histórico de pagamentos', err);
        alert('Erro ao carregar histórico de pagamentos');
      }
    });
  }

  logout() {
    localStorage.removeItem('token');
    this.router.navigate(['/login']);
  }
  pagamento() {
    this.router.navigate(['/account']);
  }
  transferencias() {
    this.router.navigate(['/transfer']);
  }
  alertas() {
    this.router.navigate(['/alerts']);
  }

  conta() {
    this.router.navigate(['/dashboard']);
  }
  
}
