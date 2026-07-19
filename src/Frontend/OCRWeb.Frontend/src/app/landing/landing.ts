import { Component, OnInit, inject } from '@angular/core';
import { Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { AuthService } from '../auth/auth.service';
import { DEFAULT_LANDING_CONTENT } from './landing-content';

@Component({
  selector: 'app-landing',
  imports: [MatButtonModule],
  templateUrl: './landing.html',
  styleUrl: './landing.scss'
})
export class Landing implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  protected readonly content = DEFAULT_LANDING_CONTENT;

  ngOnInit(): void {
    this.authService.checkSession().subscribe(() => {
      if (this.authService.isAuthenticated()) {
        this.router.navigateByUrl('/home');
      }
    });
  }

  protected signIn(): void {
    this.authService.login('/home');
  }
}
