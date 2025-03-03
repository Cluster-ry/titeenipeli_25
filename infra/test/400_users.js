import http from "k6/http";
import { check, sleep } from "k6";

export const options = {
  scenarios: {
    contacts: {
      executor: "per-vu-iterations",
      vus: 400,
      iterations: 1,
      maxDuration: "180s",
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

  const authCookie = authRes.cookies["X-Authorization"]
    ? authRes.cookies["X-Authorization"][0].value
    : null;

  if (!authCookie) {
    console.error(`VU ${userId} failed to get authentication cookie`);
    return;
  }

  const x = 0;
  const authHeaders = {
    "Content-Type": "application/json",
    Cookie: `X-Authorization=${authCookie}`,
  };

  const currentRes = http.get(
    "https://peli.test.cluster2017.fi/api/v1/users/current",
    {
      headers: authHeaders,
    }
  );

  sleep(30);

  const guild = authRes.body.guild;

  if (guild < 6) {
    for (let i = 0; i < 200; i++) {
      const pixelPayload = JSON.stringify({ x: x, y: i });

      const pixelRes = http.post(
        "https://peli.test.cluster2017.fi/api/v1/state/map/pixels",
        pixelPayload,
        {
          headers: authHeaders,
        }
      );

      check(pixelRes, {
        "Pixel post succeeded": (res) => res.status === 200,
      });

      sleep(10);
    }
  } else {
    for (let i = 0; i > -200; i--) {
      const pixelPayload = JSON.stringify({ x: x, y: i });

      const pixelRes = http.post(
        "https://peli.test.cluster2017.fi/api/v1/state/map/pixels",
        pixelPayload,
        {
          headers: authHeaders,
        }
      );
      if (pixelRes.status !== 200) {
        i++;
      }

      sleep(10);
    }
  }
}
