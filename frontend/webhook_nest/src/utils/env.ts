export function getEnvOrNull(key: string): string {
  const value = import.meta.env[key];

  if (!value || value.trim() === '') {
    throw new Error(
      `Environment variable '${key}' is required but not set or empty`
    );
  }

  return value.trim();
}

export function getEnvWithValidation<T = string>(
  key: string,
  validator?: (value: string) => T,
  defaultValue?: T
): T | null {
  const value = import.meta.env[key];

  if (!value || value.trim() === '') {
    if (defaultValue !== undefined) {
      return defaultValue;
    }
    return null;
  }

  const trimmedValue = value.trim();

  if (validator) {
    try {
      return validator(trimmedValue);
    } catch (error) {
      throw new Error(
        `Environment variable '${key}' validation failed: ${error}`
      );
    }
  }

  return trimmedValue as T;
}

export const validators = {
  port: (value: string): number => {
    const port = parseInt(value, 10);
    if (isNaN(port) || port < 1 || port > 65535) {
      throw new Error(`Invalid port number: ${value}`);
    }
    return port;
  },

  url: (value: string): string => {
    try {
      new URL(value);
      return value;
    } catch {
      throw new Error(`Invalid URL: ${value}`);
    }
  },

  email: (value: string): string => {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(value)) {
      throw new Error(`Invalid email: ${value}`);
    }
    return value;
  },

  boolean: (value: string): boolean => {
    const lower = value.toLowerCase();
    if (lower === 'true' || lower === '1') return true;
    if (lower === 'false' || lower === '0') return false;
    throw new Error(`Invalid boolean value: ${value}`);
  },

  json: <T>(value: string): T => {
    try {
      return JSON.parse(value);
    } catch {
      throw new Error(`Invalid JSON: ${value}`);
    }
  },
};
