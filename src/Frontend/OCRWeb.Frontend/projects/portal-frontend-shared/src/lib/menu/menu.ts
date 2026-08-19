import { Component, input, output } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MenuItem } from './menu-item';

/**
 * Icon-triggered dropdown menu (e.g. a per-row "more actions" menu). Owns no text —
 * ariaLabel and every item's label come from the consuming app. Emits the selected
 * item's id rather than its label, so callers don't have to match on display text.
 */
@Component({
  selector: 'lib-menu',
  imports: [MatButtonModule, MatIconModule, MatMenuModule],
  templateUrl: './menu.html',
  styleUrl: './menu.scss'
})
export class Menu {
  readonly icon = input('more_vert');
  readonly ariaLabel = input.required<string>();
  readonly items = input.required<MenuItem[]>();
  readonly itemSelected = output<string>();

  protected onItemClick(id: string): void {
    this.itemSelected.emit(id);
  }
}
