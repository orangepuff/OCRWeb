import { Component, input, output } from '@angular/core';
import { MatTabsModule } from '@angular/material/tabs';

/**
 * Generic tab strip. Owns no text (tab labels come from the consuming app) and no
 * content — it only tracks which tab is selected. The consuming app renders each
 * tab's panel itself (e.g. based on selectedIndex), since that content is always
 * app-specific and doesn't belong in a shared package.
 */
@Component({
  selector: 'lib-tab-group',
  imports: [MatTabsModule],
  templateUrl: './tab-group.html',
  styleUrl: './tab-group.scss'
})
export class TabGroup {
  readonly tabs = input.required<string[]>();
  readonly selectedIndex = input(0);
  readonly selectedIndexChange = output<number>();

  protected onSelectedIndexChange(index: number): void {
    this.selectedIndexChange.emit(index);
  }
}
