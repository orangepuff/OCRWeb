import { Component, inject, OnInit } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { Avatar } from '@orangepuff/portal-frontend-shared';
import { AuthService } from '../auth/auth.service';
import { PORTAL_SHELL_CONFIG } from '../config/portal-shell-config';
import { TranslatePipe } from '../translation/translate.pipe';

@Component({
  selector: 'lib-portal-header',
  imports: [RouterLink, MatButtonModule, MatIconModule, MatMenuModule, Avatar, TranslatePipe],
  templateUrl: './header.html',
  styleUrl: './header.scss'
})
export class PortalHeader implements OnInit {
  protected readonly authService = inject(AuthService);
  protected readonly config = inject(PORTAL_SHELL_CONFIG);
  protected readonly module = 'OrangepuffPortal.Frontend';
  private readonly router = inject(Router);

  ngOnInit(): void {
    this.authService.checkSession().subscribe();
  }

  protected signOut(): void {
    this.authService.logout().subscribe(() => this.router.navigateByUrl('/'));
  }
}
