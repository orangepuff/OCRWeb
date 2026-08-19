import { Component, input } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

/**
 * Generic Material text field bound directly to a caller-owned FormControl. Owns no text —
 * label/placeholder/hint/errorText all come from the consuming app's own label/message files.
 */
@Component({
  selector: 'lib-text-input',
  imports: [ReactiveFormsModule, MatFormFieldModule, MatInputModule],
  templateUrl: './text-input.html',
  styleUrl: './text-input.scss'
})
export class TextInput {
  readonly control = input.required<FormControl<string>>();
  readonly label = input.required<string>();
  readonly placeholder = input<string | null>(null);
  readonly type = input<'text' | 'email' | 'password'>('text');
  readonly errorText = input<string | null>(null);
  readonly hint = input<string | null>(null);
}
