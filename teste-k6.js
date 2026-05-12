import http from 'k6/http';
import { check, sleep } from 'k6';

// 1. Configuração do Comportamento (Os Estágios)
export const options = {
  stages: [
    { duration: '30s', target: 50 },  // Rampa de subida: Vai de 0 a 50 usuários em 30s
    { duration: '1m', target: 50 },   // Platô: Mantém 50 usuários batendo por 1 minuto
    { duration: '10s', target: 0 },   // Rampa de descida: Reduz para 0 usuários em 10s
  ],
};

// 2. O que cada "Usuário Virtual" vai fazer
export default function () {
  // Bate na sua API rodando dentro do Kubernetes
  const res = http.get('http://cadastro-api.api.svc.cluster.local:80/api/Usuario');
  
  check(res, {
    'status foi 200': (r) => r.status === 200,
    'respondeu rápido (menos de 500ms)': (r) => r.timings.duration < 500,
  });

  sleep(1);
}