import { CommonModule } from '@angular/common';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';

@Component({
  selector: 'app-transfer',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './transfer.component.html',
  styleUrl: './transfer.component.scss'
})
export class TransferComponent implements OnInit {
  transferForm: FormGroup;
  historico: any[] = [];
  contas: any[] = [];
  contaSelecionada: any = null;
  movimentos: any[] = [];
  mensagemErro: string = '';
  nomeUtilizador: string = '';

  private baseApiUrl = 'https://localhost:7186/api';

  constructor(
    private fb: FormBuilder,
    private http: HttpClient,
    private router: Router
  ) {
    this.transferForm = this.fb.group({
      ibanOrigem: ['', Validators.required],
      ibanDestino: ['', Validators.required],
      valor: [0, [Validators.required, Validators.min(0.01)]],
      descricao: ['']  
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
    return new HttpHeaders({
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    });
  }

  carregarContas() {
    const headers = this.getAuthHeaders();

    this.http.get<any[]>(`${this.baseApiUrl}/ContaBancaria/minhascontas`, { headers }).subscribe({
      next: (contas) => {
        this.contas = contas;
        if (contas.length > 0) {
          this.contaSelecionada = contas[0];
          this.transferForm.patchValue({ ibanOrigem: this.contaSelecionada.iban });
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

  enviarTransferencia() {
    if (this.transferForm.valid) {
      const raw = this.transferForm.value;

      const payload = {
        IBANOrigem: raw.ibanOrigem,
        IBANDestino: raw.ibanDestino,
        Valor: raw.valor,
        Descricao: raw.descricao || ''
      };

      console.log('Payload enviado:', payload);

      this.http.post(`${this.baseApiUrl}/Transferencia`, payload, {
        headers: this.getAuthHeaders()
      }).subscribe({
        next: () => {
          alert('Transferência realizada com sucesso!');
          this.transferForm.reset();
          this.carregarHistorico();
        },
        error: (err) => {
          console.error('Erro ao realizar transferência', err);
          alert('Erro ao realizar transferência');
        }
      });
    } else {
      alert('Preencha corretamente todos os campos obrigatórios.');
    }
  }

  carregarHistorico() {
    this.http.get<any[]>(`${this.baseApiUrl}/Transferencia/minhastransferencias`, {
      headers: this.getAuthHeaders()
    }).subscribe({
      next: (data) => this.historico = data,
      error: (err) => {
        console.error('Erro ao carregar histórico de transferências', err);
        alert('Erro ao carregar histórico de transferências');
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
