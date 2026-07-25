import { Component, OnInit, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Button } from '@orangepuff/portal-frontend-shared';
import { I18nService } from '../../i18n/i18n.service';
import { ProjectListItem } from '../../models/project-list-item';
import { ProjectService } from '../../services/project.service';

@Component({
  selector: 'app-project-list',
  imports: [Button],
  templateUrl: './project-list.html',
  styleUrl: './project-list.scss'
})
export class ProjectList implements OnInit {
  private readonly projectService = inject(ProjectService);
  private readonly router = inject(Router);

  protected readonly i18n = inject(I18nService);
  protected readonly projects = signal<ProjectListItem[]>([]);
  protected readonly loaded = signal(false);

  ngOnInit(): void {
    this.projectService.list().subscribe((projects) => {
      this.projects.set(projects);
      this.loaded.set(true);
    });
  }

  protected addProject(): void {
    this.router.navigate(['/projects/add']);
  }
}
