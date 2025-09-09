# Use the official Node.js 18 Alpine image as base
FROM node:18-alpine AS base

# Set the working directory
WORKDIR /app

# Install system dependencies
RUN apk add --no-cache libc6-compat curl

# Install dependencies only when needed
FROM base AS deps
WORKDIR /app

# Copy package files
COPY package*.json ./

# Install all dependencies for build
RUN npm ci --frozen-lockfile

# Build stage
FROM base AS builder
WORKDIR /app

# Copy dependencies
COPY --from=deps /app/node_modules ./node_modules

# Copy source code
COPY . .

# Generate Prisma client if needed and build the application
RUN npm run build

# Production dependencies
FROM base AS prod-deps
WORKDIR /app

# Copy package files
COPY package*.json ./

# Install only production dependencies
RUN npm ci --only=production --frozen-lockfile && npm cache clean --force

# Development stage
FROM base AS dev
WORKDIR /app

# Copy all dependencies
COPY --from=deps /app/node_modules ./node_modules

# Copy source code
COPY . .

# Create logs directory
RUN mkdir -p logs && chown -R node:node logs

# Use node user for security
USER node

# Expose the port
EXPOSE 3001

# Set development environment
ENV NODE_ENV=development

# Start in development mode
CMD ["npm", "run", "start:dev"]

# Production stage
FROM base AS runner
WORKDIR /app

# Create a non-root user to run the app
RUN addgroup --system --gid 1001 nodejs && \
    adduser --system --uid 1001 nestjs --ingroup nodejs

# Create logs directory with proper permissions
RUN mkdir -p logs && chown nestjs:nodejs logs

# Copy built application from builder stage
COPY --from=builder --chown=nestjs:nodejs /app/dist ./dist

# Copy production dependencies
COPY --from=prod-deps --chown=nestjs:nodejs /app/node_modules ./node_modules

# Copy necessary files
COPY --from=builder --chown=nestjs:nodejs /app/package.json ./package.json

# Switch to non-root user
USER nestjs

# Expose the port
EXPOSE 3001

# Set environment to production
ENV NODE_ENV=production
ENV PORT=3001

# Health check using the health endpoint
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
  CMD curl -f http://localhost:3001/health/liveness || exit 1

# Start the application
CMD ["node", "dist/main"]
