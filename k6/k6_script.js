import { sleep, check } from 'k6';
import http from 'k6/http';
import { randomIntBetween } from 'https://jslib.k6.io/k6-utils/1.2.0/index.js';

const pageSize = 100;
const baseUrl = 'https://wordflux-api.azurewebsites.net'


/* stages: [
  { duration: '30s', target: 50 },
  { duration: '2m', target: 500 }, // simulate ramp-up of traffic from 1 to 100 users over 5 minutes.
  { duration: '1m', target: 0 }, // ramp-down to 0 users
], */

export const options = {
  vus: 10,
  duration: '30s',
  thresholds: {
    http_req_failed: ['rate<0.01'], // http errors should be less than 1%
    http_req_duration: ['p(99)<300'], // 99% of requests must complete below 1.5s
  },
};

export function setup() {

}


export default function () {

  const query = `${baseUrl}/health`;
  const res = http.get(query);

  check(res, { 'status was 200': (r) => r.status == 200 });
  sleep(1);
}