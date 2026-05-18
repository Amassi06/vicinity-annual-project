import express, { type Express } from 'express';
import { healthRouter } from './routes/health.js';
import { authRouter } from './routes/auth.js';

export function createApp(): Express {
  const app = express();
  app.disable('x-powered-by');
  app.use(express.json({ limit: '1mb' }));
  app.use(healthRouter);
  app.use(authRouter);
  return app;
}
