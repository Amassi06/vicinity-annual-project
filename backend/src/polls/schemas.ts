import { z } from 'zod';

export const PollCreateSchema = z.object({
  neighbourhoodId: z.string().uuid(),
  title: z.string().min(1).max(500),
  options: z.array(z.string().min(1).max(200)).min(2).max(16),
  closesAt: z.coerce.date().optional(),
});

export const PollVoteSchema = z.object({
  choiceIndex: z.number().int().min(0),
});
