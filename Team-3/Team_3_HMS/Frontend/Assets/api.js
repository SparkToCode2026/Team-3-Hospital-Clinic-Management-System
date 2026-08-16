/* ═══════════════════════════════════════════════════════
   MedCore HMS — Centralized API Client Helper (api.js)
   ═══════════════════════════════════════════════════════ */

const API_PORTS = [5251, 5000, 7286];
let detectedBaseUrl = 'http://localhost:5251/api';

function getAuthToken() {
  return localStorage.getItem('token') || sessionStorage.getItem('token') || '';
}

function getAuthUser() {
  try {
    const raw = localStorage.getItem('user') || sessionStorage.getItem('user');
    if (raw) {
      const parsed = JSON.parse(raw);
      if (parsed && typeof parsed === 'object') {
        const name = parsed.fullname || parsed.Fullname || parsed.name || parsed.fullName || '';
        const role = parsed.role || 'Doctor';
        const uid = parsed.userId || parsed.userID || parsed.id || '';
        if (name || uid) {
          return {
            userId: uid,
            fullname: name,
            role: role
          };
        }
      }
    }
    // Fallback: parse from JWT token
    const token = getAuthToken();
    if (token && token.includes('.')) {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const name = payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] || payload.unique_name || payload.name || payload.sub || '';
      const role = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || payload.role || 'Doctor';
      const uid = payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] || payload.nameid || payload.userId || '';
      return {
        userId: uid,
        fullname: name,
        role: role
      };
    }
  } catch (e) {
    console.warn('Error parsing auth user:', e);
  }
  return {};
}

async function apiFetch(endpoint, options = {}) {
  const token = getAuthToken();
  const headers = {
    'Content-Type': 'application/json',
    ...(token ? { 'Authorization': `Bearer ${token}` } : {}),
    ...(options.headers || {})
  };

  const cleanEndpoint = endpoint.startsWith('/') ? endpoint : `/${endpoint}`;

  // Try ports sequentially if needed
  let lastErr = null;
  const urlsToTry = [
    detectedBaseUrl + cleanEndpoint,
    `http://localhost:5251/api${cleanEndpoint}`,
    `http://localhost:5000/api${cleanEndpoint}`,
    `http://localhost:5251${cleanEndpoint}`,
    `http://localhost:5000${cleanEndpoint}`
  ];

  // Remove duplicates
  const uniqueUrls = [...new Set(urlsToTry)];

  for (const url of uniqueUrls) {
    try {
      const res = await fetch(url, { ...options, headers });
      if (res.status === 401) {
        console.warn('Unauthorized request. Token may be expired.');
      }

      if (res.status === 204) {
        if (url.includes('/api')) {
          detectedBaseUrl = url.substring(0, url.indexOf('/api') + 4);
        }
        return null;
      }

      let data = null;
      const text = await res.text().catch(() => '');
      if (text) {
        try {
          data = JSON.parse(text);
        } catch {
          data = text;
        }
      }

      if (!res.ok) {
        let errMessage = '';
        if (typeof data === 'string') {
          errMessage = data;
        } else if (data && typeof data === 'object') {
          if (data.message) {
            errMessage = data.message;
          } else if (data.title) {
            errMessage = data.title;
          } else if (data.errors && typeof data.errors === 'object') {
            errMessage = Object.values(data.errors).flat().join(', ');
          }
        }
        throw new Error(errMessage || res.statusText || `HTTP ${res.status}`);
      }

      // Remember successful base URL
      if (url.includes('/api')) {
        detectedBaseUrl = url.substring(0, url.indexOf('/api') + 4);
      }
      return data;
    } catch (err) {
      lastErr = err;
      // If it's a 4xx/5xx HTTP error from server response, don't retry other ports
      if (err.message && !err.message.includes('Failed to fetch') && !err.message.includes('NetworkError') && !err.message.includes('Load failed')) {
        throw err;
      }
    }
  }

  console.error(`API Error [${endpoint}]:`, lastErr?.message || 'Server connection failed');
  throw lastErr || new Error('Unable to connect to backend server on localhost.');
}
