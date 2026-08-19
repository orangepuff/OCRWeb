import { Component, input, output } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';

/**
 * Generic themed button. Owns no text — every consuming app supplies its own label.
 *
 * Variants:
 * - `primary`  → `mat-flat-button`
 * - `secondary` → `mat-button`
 * - `raised`   → `mat-raised-button`
 * - `stroked`  → `mat-stroked-button`
 */
@Component({
  selector: 'lib-button',
  imports: [MatButtonModule],
  templateUrl: './button.html',
  styleUrl: './button.scss'
})
export class Button {
  readonly label = input.required<string>();
  readonly variant = input<'primary' | 'secondary' | 'raised' | 'stroked'>('secondary');
  readonly type = input<'button' | 'submit'>('button');
  readonly disabled = input(false);
  readonly clicked = output<void>();

  protected onClick(): void {
    if (this.type() === 'button') {
      this.clicked.emit();
    }
  }
}
