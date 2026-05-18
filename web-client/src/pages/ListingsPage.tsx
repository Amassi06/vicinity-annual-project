import { FormEvent, useCallback, useEffect, useState, type ReactElement } from 'react';
import { apiFetch } from '../lib/api.js';
import { useNeighbourhoods } from '../context/NeighbourhoodContext.js';
import { NeighbourhoodSelect } from '../components/NeighbourhoodSelect.js';

type ListingDoc = {
  _id: string;
  title: string;
  kind: string;
  category: string;
  pricePoints: number;
  status: string;
};

export function ListingsPage(): ReactElement {
  const { selectedId } = useNeighbourhoods();
  const [items, setItems] = useState<ListingDoc[]>([]);
  const [title, setTitle] = useState('');
  const [kind, setKind] = useState<'offer' | 'request'>('offer');
  const [category, setCategory] = useState('services');
  const [err, setErr] = useState<string | null>(null);

  const load = useCallback(async () => {
    if (!selectedId) {
      setItems([]);
      return;
    }
    try {
      const res = await apiFetch<{ items: ListingDoc[] }>(
        `/listings?neighbourhoodId=${selectedId}&status=open`,
      );
      setItems(res.items);
    } catch (e) {
      setErr(e instanceof Error ? e.message : 'Erreur');
    }
  }, [selectedId]);

  useEffect(() => {
    void load();
  }, [load]);

  async function create(ev: FormEvent): Promise<void> {
    ev.preventDefault();
    if (!selectedId) return;
    try {
      await apiFetch('/listings', {
        method: 'POST',
        json: {
          neighbourhoodId: selectedId,
          title,
          kind,
          category,
          pricePoints: 0,
        },
      });
      setTitle('');
      await load();
    } catch (e) {
      setErr(e instanceof Error ? e.message : 'Erreur');
    }
  }

  async function accept(id: string): Promise<void> {
    await apiFetch(`/listings/${id}/accept`, { method: 'POST' });
    await load();
  }

  return (
    <section className="panel">
      <h1 style={{ marginTop: 0 }}>Annonces</h1>
      <NeighbourhoodSelect />
      {!selectedId ? (
        <p className="muted">Sélectionnez un quartier.</p>
      ) : (
        <>
          <form className="inline-form" onSubmit={(e) => void create(e)}>
            <input value={title} onChange={(e) => setTitle(e.target.value)} placeholder="Titre" required />
            <select value={kind} onChange={(e) => setKind(e.target.value as 'offer' | 'request')}>
              <option value="offer">Offre</option>
              <option value="request">Demande</option>
            </select>
            <input value={category} onChange={(e) => setCategory(e.target.value)} placeholder="Catégorie" />
            <button type="submit" className="primary">
              Créer
            </button>
          </form>
          {err ? <p className="error-msg">{err}</p> : null}
          <ul className="item-list">
            {items.map((l) => (
              <li key={l._id}>
                <strong>{l.title}</strong> ({l.kind}) — {l.pricePoints} pts
                <button type="button" className="secondary" onClick={() => void accept(l._id)}>
                  Accepter
                </button>
              </li>
            ))}
          </ul>
        </>
      )}
    </section>
  );
}
