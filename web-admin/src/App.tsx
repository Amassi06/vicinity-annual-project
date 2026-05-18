import type { ReactElement } from 'react';

export function App(): ReactElement {
  return (
    <main className="page">
      <h1>Vicinity — Admin</h1>
      <p>Back-office minimal (Vite + React). Le proxy `/api` pointe sur le backend local :3000.</p>
    </main>
  );
}
