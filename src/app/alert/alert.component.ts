import { CommonModule } from '@angular/common';
import { HttpClient, HttpHeaders  } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { Router, RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-alert',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './alert.component.html',
  styleUrl: './alert.component.scss'
})
export class AlertComponent implements OnInit {
  alertas: any[] = [];
  notificacoes: any[] = [];
  nomeUtilizador: string = '';

  constructor(private http: HttpClient, private router: Router) {}

  ngOnInit(): void {
    const nome = localStorage.getItem('nomeUtilizador');
    this.nomeUtilizador = nome ?? 'Utilizador';
    this.carregarNotificao();
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

  carregarNotificao() {
    const headers = this.getAuthHeaders();
  
    this.http.get<any[]>(`https://localhost:7186/api/Notificacao/naolidas`, { headers }).subscribe({
      next: (notificacao) => {
        console.log('Notificações:', notificacao);
        this.notificacoes = notificacao.slice(0, 10); 
      },
      error: (err) => {
        console.error('Erro ao carregar notificações', err);
        this.notificacoes = [];
      }
    });;
  }
  marcarComoLida(notificacaoId: number) {
    const token = localStorage.getItem('token');
    if (!token) {
      alert('Utilizador não autenticado.');
      return;
    }

    const headers = new HttpHeaders({
      'Authorization': `Bearer ${token}`
    });

    
    this.http.post(`https://localhost:7186/api/Notificacao/${notificacaoId}/marcarcomolida`, {}, { headers, responseType: 'text' })
      .subscribe({
        next: () => {
          alert('Notificação marcada como lida.');
          this.carregarNotificao(); 
        },
        error: () => alert('Erro ao marcar notificação como lida.')
      });
  }


logout() {
    localStorage.removeItem('token');
    this.router.navigate(['/login']);
  }
  dash() {
    this.router.navigate(['/dashboard']);
  }
  pagamento() {
    this.router.navigate(['/account']);
  }
  transferencias() {
    this.router.navigate(['/transfer']);
  }
  
  conta() {
    this.router.navigate(['/dashboard']);
  }
}
 

