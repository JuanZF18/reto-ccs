import http from 'k6/http';
import { check } from 'k6';

export const options = {
  // Escenario: Carga constante
  scenarios: {
    constant_request_rate: {
      executor: 'constant-arrival-rate',
      rate: 600,
      timeUnit: '1s', 
      duration: '2m', // Duración: 2 Minutos
      preAllocatedVUs: 50, // Usuarios listos
      maxVUs: 100,
    },
  },
};

export default function () {
  const url = 'http://host.docker.internal:8080/api/telemetry';

  const payload = JSON.stringify({
    plate: "STRESS-TEST",
    speed: Math.random() * 120,
    lat: 6.2,
    lon: -75.5,
    heading: 0,
    metadata: {}
  });

  const params = {
    headers: {
      'Content-Type': 'application/json',
    },
  };

  const res = http.post(url, payload, params);

  check(res, {
    'status is 202': (r) => r.status === 202,
  });
}