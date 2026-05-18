import { Router, type Request, type Response } from 'express';
import { z } from 'zod';
import * as authService from '../../auth/service.js';
import { requireAuth, requireRole } from '../../auth/middleware.js';
import { logger } from '../../logger/index.js';

const router = Router();

const SignupSchema = z.object({
  email: z.string().email(),
  password: z.string().min(8).max(128),
  displayName: z.string().min(1).max(120),
});

const LoginSchema = z.object({
  email: z.string().email(),
  password: z.string().min(1).max(128),
});

const RefreshSchema = z.object({
  refreshToken: z.string().min(1),
});

router.post('/auth/signup', async (req: Request, res: Response) => {
  const parsed = SignupSchema.safeParse(req.body);
  if (!parsed.success) {
    res.status(400).json({ error: 'invalid_payload', details: parsed.error.flatten() });
    return;
  }
  try {
    const result = await authService.signup(parsed.data);
    res.status(201).json(result);
  } catch (err: unknown) {
    if (err instanceof Error && err.message.includes('Unique constraint')) {
      res.status(409).json({ error: 'email_already_registered' });
      return;
    }
    logger.error({ err }, 'signup failed');
    res.status(500).json({ error: 'internal_error' });
  }
});

router.post('/auth/login', async (req: Request, res: Response) => {
  const parsed = LoginSchema.safeParse(req.body);
  if (!parsed.success) {
    res.status(400).json({ error: 'invalid_payload' });
    return;
  }
  try {
    const result = await authService.login(parsed.data.email, parsed.data.password);
    res.status(200).json(result);
  } catch {
    res.status(401).json({ error: 'invalid_credentials' });
  }
});

router.post('/auth/refresh', async (req: Request, res: Response) => {
  const parsed = RefreshSchema.safeParse(req.body);
  if (!parsed.success) {
    res.status(400).json({ error: 'invalid_payload' });
    return;
  }
  try {
    const tokens = await authService.refresh(parsed.data.refreshToken);
    res.status(200).json(tokens);
  } catch {
    res.status(401).json({ error: 'invalid_session' });
  }
});

router.post('/auth/logout', async (req: Request, res: Response) => {
  const parsed = RefreshSchema.safeParse(req.body);
  if (!parsed.success) {
    res.status(400).json({ error: 'invalid_payload' });
    return;
  }
  await authService.logout(parsed.data.refreshToken);
  res.status(204).send();
});

router.get('/auth/me', requireAuth, (req: Request, res: Response) => {
  res.status(200).json(req.auth);
});

router.get('/auth/admin-only', requireAuth, requireRole('ADMIN'), (_req, res) => {
  res.status(200).json({ ok: true });
});

export const authRouter = router;
