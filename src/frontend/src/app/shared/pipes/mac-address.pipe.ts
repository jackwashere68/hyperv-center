import { Pipe, PipeTransform } from '@angular/core';

@Pipe({ name: 'macAddress' })
export class MacAddressPipe implements PipeTransform {
  transform(value: string): string {
    if (!value) return '—';
    // If already formatted (contains : or -), return as-is
    if (value.includes(':') || value.includes('-')) return value;
    // Format raw "00155DFE1A03" as "00:15:5D:FE:1A:03"
    return value.match(/.{1,2}/g)?.join(':') ?? value;
  }
}
