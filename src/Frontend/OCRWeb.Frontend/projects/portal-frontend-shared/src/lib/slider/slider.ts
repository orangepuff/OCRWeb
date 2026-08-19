import { Component, input } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatSliderModule } from '@angular/material/slider';

/**
 * Generic numeric slider bound directly to a caller-owned FormControl. Owns no text —
 * label comes from the consuming app's own label files.
 */
@Component({
  selector: 'lib-slider',
  imports: [ReactiveFormsModule, MatSliderModule],
  templateUrl: './slider.html',
  styleUrl: './slider.scss'
})
export class Slider {
  readonly control = input.required<FormControl<number>>();
  readonly label = input.required<string>();
  readonly min = input(0);
  readonly max = input.required<number>();
  readonly step = input(1);
}
