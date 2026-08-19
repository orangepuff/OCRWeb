import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { TranslatePipe } from '../translation/translate.pipe';

@Component({
  selector: 'lib-portal-unauthorized-page',
  imports: [RouterLink, MatButtonModule, TranslatePipe],
  templateUrl: './unauthorized-page.html',
  styleUrl: './unauthorized-page.scss'
})
export class UnauthorizedPage {
  protected readonly module = 'OrangepuffPortal.Frontend';
}
