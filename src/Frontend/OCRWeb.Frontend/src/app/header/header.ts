import { Component, inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../auth/auth.service';

@Component({
  selector: 'app-header',
  imports: [],
  templateUrl: './header.html',
  styleUrl: './header.scss'
})
export class Header implements OnInit {
  protected readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  ngOnInit(): void {
    this.authService.checkSession().subscribe();
  }

  protected signIn(): void {
    this.authService.login('/home');
  }

  protected signOut(): void {
    this.authService.logout().subscribe(() => this.router.navigateByUrl('/'));
  }
}