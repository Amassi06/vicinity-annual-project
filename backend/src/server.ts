import { createApp } from './http/app.js';
import { env } from './config/env.js';
import { logger } from './logger/index.js';
import { prisma } from './db/prisma.js';
import { connectMongo, disconnectMongo } from './db/mongo/connection.js';
import { getNeo4jDriver, closeNeo4j } from './db/neo4j/driver.js';
import { applyNeo4jConstraints } from './db/neo4j/constraints.js';

async function bootstrap(): Promise<void> {
  await prisma.$connect();
  await connectMongo();
  getNeo4jDriver();
  await applyNeo4jConstraints();

  const app = createApp();
  const server = app.listen(env.PORT, () => {
    logger.info({ port: env.PORT, env: env.NODE_ENV }, 'backend listening');
  });

  const shutdown = (signal: NodeJS.Signals): void => {
    logger.info({ signal }, 'shutting down');
    server.close(() => {
      void (async () => {
        await disconnectMongo();
        await closeNeo4j();
        await prisma.$disconnect();
        process.exit(0);
      })();
    });
    setTimeout(() => process.exit(1), 10_000).unref();
  };
  process.on('SIGINT', shutdown);
  process.on('SIGTERM', shutdown);
}

bootstrap().catch((err: unknown) => {
  logger.fatal({ err }, 'failed to bootstrap backend');
  process.exit(1);
});
