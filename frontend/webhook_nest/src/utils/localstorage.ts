export function setItem(key: string, value: any): void {
  const serializedValue = JSON.stringify(value);
  localStorage.setItem(key, serializedValue);
}

export function getItem<T>(key: string): T | null {
  const item = localStorage.getItem(key);
  return item ? JSON.parse(item) : null;
}

export function removeItem(key: string): void {
  localStorage.removeItem(key);
}

export function clear(): void {
  localStorage.clear();
}

export function hasItem(key: string): boolean {
  return localStorage.getItem(key) !== null;
}

export function getKeys(): string[] {
  return Object.keys(localStorage);
}
