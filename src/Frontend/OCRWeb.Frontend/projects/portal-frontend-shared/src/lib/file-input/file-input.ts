import { Component, input, output } from '@angular/core';

/**
 * Generic file picker. Owns no text — label comes from the consuming app's own label
 * files. Emits the selected File (or null when cleared) rather than integrating with
 * Reactive Forms — native file inputs have no meaningful string value to bind to a
 * FormControl the way TextInput does.
 */
@Component({
  selector: 'lib-file-input',
  imports: [],
  templateUrl: './file-input.html',
  styleUrl: './file-input.scss'
})
export class FileInput {
  readonly label = input.required<string>();
  readonly accept = input<string | null>(null);
  readonly disabled = input(false);
  readonly fileSelected = output<File | null>();

  protected onChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.fileSelected.emit(input.files?.[0] ?? null);
  }
}
