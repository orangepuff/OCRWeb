import { Component, inject, OnInit } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { Avatar } from 'ocrweb.frontend.shared';
import { AuthService } from '../auth/auth.service';

@Component({
  selector: 'app-header',
  imports: [RouterLink, MatButtonModule, MatIconModule, MatMenuModule, Avatar],
  templateUrl: './header.html',
  styleUrl: './header.scss'
})
export class Header implements OnInit {
  protected readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  ngOnInit(): void {
    this.authService.checkSession().subscribe();
  }

  protected signOut(): void {
    this.authService.logout().subscribe(() => this.router.navigateByUrl('/'));
  }
}
