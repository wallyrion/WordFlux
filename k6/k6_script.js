import { sleep } from 'k6';
import http from 'k6/http';
import { randomIntBetween } from 'https://jslib.k6.io/k6-utils/1.2.0/index.js';

const pageSize = 100;
const baseUrl = 'https://localhost:7443'

export const options = {
  stages: [
    { duration: '30s', target: 50 },
    { duration: '2m', target: 5000 }, // simulate ramp-up of traffic from 1 to 100 users over 5 minutes.
    { duration: '1m', target: 0 }, // ramp-down to 0 users
  ],
  thresholds: {
    'http_req_duration': ['p(99)<1500'], // 99% of requests must complete below 1.5s
  },
};

export function setup() {

}


export default function () {

  const query = `${baseUrl}/health`;
  http.get(query);
}