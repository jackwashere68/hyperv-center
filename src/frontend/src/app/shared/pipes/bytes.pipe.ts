import { Pipe, PipeTransform } from '@angular/core';

@Pipe({ name: 'bytes' })
export class BytesPipe implements PipeTransform {
  transform(bytes: number): string {
    if (bytes === 0) return '0 B';
    const units = ['B', 'KB', 'MB', 'GB', 'TB'];
    const i = Math.floor(Math.log(bytes) / Math.log(1024));
    const value = bytes / Math.pow(1024, i);
    return `${Number.isInteger(value) ? value : value.toFixed(1)} ${units[i]}`;
  }
}
