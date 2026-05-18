import type { ReactElement } from 'react';

export function App(): ReactElement {
  return (
    <main className="page">
      <h1>Vicinity</h1>
      <p>Front habitant minimal (Vite + React). Proxifie `/api` vers le backend sur le port 3000.</p>
    </main>
  );
}
