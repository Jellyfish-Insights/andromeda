import { fetch } from 'domain-task';

export function authenticatedFetch(url: string | Request, init?: RequestInit) {
  return fetchOrTimeout(url, Object.assign(init || {}, { credentials: 'same-origin' }));
}

type Milliseconds = number;

// adapted from: https://stackoverflow.com/a/49857905
function fetchOrTimeout(url: string | Request, init?: RequestInit, timeout: Milliseconds = 30000) {
  var running = true
  return Promise.race([
    fetch(url, init).then(x => {
      running = false
      return x
    })
    ,
    new Promise((_, reject) =>
      setTimeout(() => {
        if (running && confirm("Request: \"" + url + "\" is taking too long do you want to reload")) {
          location.reload()
          reject(new Error('timeout'))
        }
      }, timeout)
    )
  ]);
}
