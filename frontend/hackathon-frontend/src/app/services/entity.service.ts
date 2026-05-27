import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface Entity {
  id: number;
  name: string;
  description: string;
}

export type NewEntity = Omit<Entity, 'id'>;

@Injectable({
  providedIn: 'root'
})
export class EntityService {

  apiUrl = `${environment.apiBaseUrl}/api/Entity`;

  constructor(private http: HttpClient) { }

  getEntities(): Observable<Entity[]> {
    return this.http.get<Entity[]>(this.apiUrl);
  }

  addEntity(entity: Entity | NewEntity): Observable<Entity> {
    return this.http.post<Entity>(this.apiUrl, entity);
  }
}
