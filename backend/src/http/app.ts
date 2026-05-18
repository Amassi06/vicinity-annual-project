import express, { type Express } from 'express';
import { healthRouter } from './routes/health.js';
import { authRouter } from './routes/auth.js';
import { neighbourhoodRouter } from './routes/neighbourhoods.js';
import { walletRouter } from './routes/wallet.js';
import { listingsRouter } from './routes/listings.js';
import { documentsRouter } from './routes/documents.js';

export function createApp(): Express {
  const app = express();
  app.disable('x-powered-by');
  app.use(express.json({ limit: '1mb' }));
  app.use(healthRouter);
  app.use(authRouter);
  app.use(neighbourhoodRouter);
  app.use(walletRouter);
  app.use(listingsRouter);
  app.use(documentsRouter);
  return app;
}
