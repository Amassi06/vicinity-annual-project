import mongoose from 'mongoose';
import { env } from '../../config/env.js';
import { logger } from '../../logger/index.js';

let connectionPromise: Promise<typeof mongoose> | null = null;

export function connectMongo(): Promise<typeof mongoose> {
  if (!connectionPromise) {
    mongoose.connection.on('connected', () => logger.info('mongo connected'));
    mongoose.connection.on('error', (err) => logger.error({ err }, 'mongo error'));
    mongoose.connection.on('disconnected', () => logger.warn('mongo disconnected'));

    connectionPromise = mongoose.connect(env.MONGO_URL, {
      serverSelectionTimeoutMS: 5_000,
      maxPoolSize: 20,
    });
  }
  return connectionPromise;
}

export async function disconnectMongo(): Promise<void> {
  if (mongoose.connection.readyState !== mongoose.ConnectionStates.disconnected) {
    await mongoose.disconnect();
    connectionPromise = null;
  }
}

export async function checkMongoHealth(): Promise<boolean> {
  try {
    if (
      mongoose.connection.readyState !== mongoose.ConnectionStates.connected ||
      !mongoose.connection.db
    ) {
      return false;
    }
    const result = await mongoose.connection.db.admin().ping();
    return result.ok === 1;
  } catch (err) {
    logger.warn({ err }, 'mongo healthcheck failed');
    return false;
  }
}
