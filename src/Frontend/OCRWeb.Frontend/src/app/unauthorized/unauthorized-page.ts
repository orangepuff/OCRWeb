import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-unauthorized-page',
  imports: [RouterLink, MatButtonModule],
  templateUrl: './unauthorized-page.html',
  styleUrl: './unauthorized-page.scss'
})
export class UnauthorizedPage {}
