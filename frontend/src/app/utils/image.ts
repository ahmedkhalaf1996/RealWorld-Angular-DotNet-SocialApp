const DEFAULT_AVATAR = 'https://cdn-icons-png.flaticon.com/512/3177/3177440.png';

export function resolveImageSrc(source?: string | null, fallback: string = DEFAULT_AVATAR): string {
  const trimmed = (source ?? '').trim();
  if(!trimmed) {
    return fallback;
  }

  if(
    trimmed.startsWith('data:') ||
    trimmed.startsWith('http://') ||
    trimmed.startsWith('https://') ||
    trimmed.startsWith('blob:')

  ){
    return trimmed;
  }


  if(/^base64[\s,]/i.test(trimmed)){
    const payload = trimmed.replace(/^base64[\s,]+/i, '');
    return `data:image/png;base64,${payload}`;
  }

  if (/^[A-Za-z0-9+/=]+$/.test(trimmed) && trimmed.length > 100) {
    return `data:image/png;base64,${trimmed}`;
  }

  return trimmed;
}
