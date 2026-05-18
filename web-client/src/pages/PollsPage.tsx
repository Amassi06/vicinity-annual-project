import { FormEvent, useCallback, useEffect, useState, type ReactElement } from 'react';
import { apiFetch } from '../lib/api.js';
import { useNeighbourhoods } from '../context/NeighbourhoodContext.js';
import { NeighbourhoodSelect } from '../components/NeighbourhoodSelect.js';

type PollDoc = { _id: string; title: string; options: string[]; status: string };

export function PollsPage(): ReactElement {
  const { selectedId } = useNeighbourhoods();
  const [items, setItems] = useState<PollDoc[]>([]);
  const [title, setTitle] = useState('');
  const [optA, setOptA] = useState('Oui');
  const [optB, setOptB] = useState('Non');
  const [err, setErr] = useState<string | null>(null);

  const load = useCallback(async () => {
    if (!selectedId) {
      setItems([]);
      return;
    }
    try {
      const res = await apiFetch<{ items: PollDoc[] }>(`/polls?neighbourhoodId=${selectedId}`);
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
      await apiFetch('/polls', {
        method: 'POST',
        json: { neighbourhoodId: selectedId, title, options: [optA, optB] },
      });
      setTitle('');
      await load();
    } catch (e) {
      setErr(e instanceof Error ? e.message : 'Erreur');
    }
  }

  async function vote(pollId: string, choiceIndex: number): Promise<void> {
    await apiFetch(`/polls/${pollId}/vote`, { method: 'POST', json: { choiceIndex } });
    await load();
  }

  return (
    <section className="panel">
      <h1 style={{ marginTop: 0 }}>Sondages</h1>
      <NeighbourhoodSelect />
      {!selectedId ? (
        <p className="muted">Sélectionnez un quartier.</p>
      ) : (
        <>
          <form className="inline-form" onSubmit={(e) => void create(e)}>
            <input value={title} onChange={(e) => setTitle(e.target.value)} placeholder="Question" required />
            <input value={optA} onChange={(e) => setOptA(e.target.value)} placeholder="Option A" />
            <input value={optB} onChange={(e) => setOptB(e.target.value)} placeholder="Option B" />
            <button type="submit" className="primary">
              Créer
            </button>
          </form>
          {err ? <p className="error-msg">{err}</p> : null}
          <ul className="item-list">
            {items.map((p) => (
              <li key={p._id}>
                <strong>{p.title}</strong>
                <div className="row-actions">
                  {p.options.map((o, i) => (
                    <button key={o} type="button" className="secondary" onClick={() => void vote(p._id, i)}>
                      {o}
                    </button>
                  ))}
                </div>
              </li>
            ))}
          </ul>
        </>
      )}
    </section>
  );
}
