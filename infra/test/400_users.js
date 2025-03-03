import http from "k6/http";
import { check, sleep } from "k6";

export const options = {
  scenarios: {
    contacts: {
      executor: "per-vu-iterations",
      vus: 390,
      iterations: 1,
      maxDuration: "500s",
    },
  },
};

export default function () {
  const userId = __VU;
  const authPayload = JSON.stringify({ token: userId });

  const authRes = http.post(
    "https://peli.test.cluster2017.fi/api/v1/users/authenticate",
    authPayload,
    {
      headers: { "Content-Type": "application/json" },
    }
  );

  check(authRes, {
    "Authentication succeeded": (res) => res.status === 200,
  });

  const currentRes = http.get(
    "https://peli.test.cluster2017.fi/api/v1/users/current"
  );

  sleep(1);

  const guild = currentRes.json().guild;
  const x = 0;

  if (guild < 6) {
    for (let i = 1; i < 200; i++) {
      const pixelRes = http.post(
        "https://peli.test.cluster2017.fi/api/v1/state/map/pixels",
        JSON.stringify({ x: x, y: i }),
        {
          headers: { "Content-Type": "application/json" },
        }
      );

      check(pixelRes, {
        "Pixel post succeeded": (res) => res.status === 200,
      });

      let number = Math.floor(Math.random() * 5);
      sleep(number);
    }
  } else {
    for (let i = -1; i > -200; i--) {
      const pixelRes = http.post(
        "https://peli.test.cluster2017.fi/api/v1/state/map/pixels",
        JSON.stringify({ x: x, y: i }),
        {
          headers: { "Content-Type": "application/json" },
        }
      );
      check(pixelRes, {
        "Pixel post succeeded": (res) => res.status === 200,
      });

      let number = Math.floor(Math.random() * 5);
      sleep(number);
    }
  }
}
