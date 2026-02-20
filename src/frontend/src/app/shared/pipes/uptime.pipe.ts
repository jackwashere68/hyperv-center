import { Pipe, PipeTransform } from '@angular/core';

@Pipe({ name: 'uptime' })
export class UptimePipe implements PipeTransform {
  transform(value: string): string {
    if (!value) return '—';

    // Parse .NET TimeSpan format: "d.hh:mm:ss.fffffff" or "hh:mm:ss.fffffff"
    const match = value.match(/^(?:(\d+)\.)?(\d+):(\d+):(\d+)/);
    if (!match) return value;

    const days = parseInt(match[1] || '0', 10);
    const hours = parseInt(match[2], 10);
    const minutes = parseInt(match[3], 10);

    const parts: string[] = [];
    if (days > 0) parts.push(`${days}d`);
    if (hours > 0) parts.push(`${hours}h`);
    if (minutes > 0) parts.push(`${minutes}m`);

    return parts.length > 0 ? parts.join(' ') : '< 1m';
  }
}
