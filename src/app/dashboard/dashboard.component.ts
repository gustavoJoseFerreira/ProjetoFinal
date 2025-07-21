import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterOutlet } from '@angular/router';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [RouterOutlet, CommonModule, FormsModule],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit {
  contas: any[] = [];
  contaSelecionada: any = null;
  movimentos: any[] = [];
  mensagemErro: string = '';
  nomeUtilizador: string = '';

  private baseApiUrl = 'https://localhost:7186/api/ContaBancaria';

  constructor(
    private router: Router,
    private http: HttpClient
  ) {}

  ngOnInit() {
    const nome = localStorage.getItem('nomeUtilizador');
    this.nomeUtilizador = localStorage.getItem('nomeUtilizador') || 'Utilizador';
    this.carregarContas();
    this.carregarMovimentosDoUtilizador();
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

    this.http.get<any[]>(`${this.baseApiUrl}/minhascontas`, { headers }).subscribe({
      next: (contas) => {
        this.contas = contas;
        if (contas.length > 0) {
          this.contaSelecionada = contas[0];
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

  carregarMovimentos(iban: string) {
    const headers = this.getAuthHeaders();
  
    this.http.get<any[]>('https://localhost:7186/api/Movimento/conta/{iban}', { headers }).subscribe({
      next: (movimentos) => {
        console.log('Movimentos recebidos:', movimentos); 
        this.movimentos = movimentos.slice(0, 5);
      },
      error: (err) => {
        console.error('Erro ao carregar movimentos', err);
        this.movimentos = [];
      }
    });
  }



  tipoContaSelecionado = 'Conta Corrente';
  criarConta() {
    const headers = this.getAuthHeaders();
  
    const novaContaDto = { TipoConta: this.tipoContaSelecionado };
  
    this.http.post<any>('https://localhost:7186/api/ContaBancaria/criar', novaContaDto, { headers }).subscribe({
      next: (res) => {
        alert('Conta criada com sucesso!');
        this.carregarContas();
      },
      error: (err) => {
        console.error('Erro ao criar conta', err);
        alert('Erro ao criar conta.');
      }
    });
  }

  carregarMovimentosDoUtilizador() {
    const headers = this.getAuthHeaders();
  
    this.http.get<any[]>(`https://localhost:7186/api/Movimento/meus`, { headers }).subscribe({
      next: (movimentos) => {
        console.log('Movimentos carregados:', movimentos);
        this.movimentos = movimentos.slice(0, 5); // Mostra só os últimos 5
      },
      error: (err) => {
        console.error('Erro ao carregar movimentos', err);
        this.movimentos = [];
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
}
