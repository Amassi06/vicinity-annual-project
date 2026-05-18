import type { ReactElement } from 'react';
import { NavLink, Navigate, Outlet } from 'react-router-dom';
import { useAuth } from '../context/AuthContext.js';

export function AdminShell(): ReactElement {
  const { user, logout } = useAuth();

  return (
    <div className="shell">
      <header>
        <strong>Vicinity Admin</strong>
        <nav>
          <NavLink to="/" end className={({ isActive }) => (isActive ? 'router-active' : '')}>
            Quartiers
          </NavLink>
          {(user?.role === 'ADMIN' || user?.role === 'MODERATOR') && (
            <NavLink to="/dsl" className={({ isActive }) => (isActive ? 'router-active' : '')}>
              DSL
            </NavLink>
          )}
          {user?.role === 'ADMIN' && (
            <NavLink to="/wallet" className={({ isActive }) => (isActive ? 'router-active' : '')}>
              Crédit points
            </NavLink>
          )}
          {!user ? (
            <>
              <NavLink to="/login">Connexion</NavLink>
              <NavLink to="/register">Inscription</NavLink>
            </>
          ) : (
            <span className="user-chip">{user.email}</span>
          )}
          {user ? (
            <button type="button" className="secondary" onClick={() => void logout()}>
              Déconnexion
            </button>
          ) : null}
        </nav>
      </header>
      <main className="shell-main shell-main-admin">
        <Outlet />
      </main>
    </div>
  );
}

export function RequireAuthGate(): ReactElement {
  const { ready, user } = useAuth();
  if (!ready) return <p className="muted">Chargement…</p>;
  if (!user) return <Navigate to="/login" replace />;
  return <Outlet />;
}
