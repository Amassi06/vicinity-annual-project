import { Router } from 'express';
import { checkPostgresHealth } from '../../db/prisma.js';
import { checkMongoHealth } from '../../db/mongo/connection.js';
import { checkNeo4jHealth } from '../../db/neo4j/driver.js';

const router = Router();

router.get('/healthz', (_req, res) => {
  res.status(200).json({ status: 'ok', uptime: process.uptime() });
});

router.get('/readyz', async (_req, res) => {
  const [postgres, mongo, neo4j] = await Promise.all([
    checkPostgresHealth(),
    checkMongoHealth(),
    checkNeo4jHealth(),
  ]);

  const checks = { postgres, mongo, neo4j };
  const allReady = postgres && mongo && neo4j;

  res.status(allReady ? 200 : 503).json({
    status: allReady ? 'ready' : 'degraded',
    checks,
  });
});

export const healthRouter = router;
