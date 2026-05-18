import crypto from 'node:crypto';
import { promises as fs } from 'node:fs';
import path from 'node:path';
import { env } from '../config/env.js';

export interface StoredFile {
  storageKey: string;
  absolutePath: string;
  sha256: string;
  bytes: number;
}

function root(): string {
  return path.resolve(env.STORAGE_DIR);
}

export async function ensureStorageDir(): Promise<void> {
  await fs.mkdir(root(), { recursive: true });
}

/**
 * Persiste un buffer sur disque sous `<STORAGE_DIR>/<sha256>.pdf`.
 * Le content-addressable storage rend les uploads idempotents (mêmes octets =
 * même fichier, pas de duplication).
 */
export async function saveBuffer(buffer: Buffer): Promise<StoredFile> {
  await ensureStorageDir();
  const sha256 = crypto.createHash('sha256').update(buffer).digest('hex');
  const storageKey = `${sha256}.pdf`;
  const absolutePath = path.join(root(), storageKey);
  try {
    await fs.access(absolutePath);
  } catch {
    await fs.writeFile(absolutePath, buffer);
  }
  return { storageKey, absolutePath, sha256, bytes: buffer.length };
}

export function resolveStoragePath(storageKey: string): string {
  return path.join(root(), storageKey);
}

export async function readStoredFile(storageKey: string): Promise<Buffer> {
  return fs.readFile(resolveStoragePath(storageKey));
}
