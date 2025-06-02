import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import { CharacterDto } from '../models/character.model';

@Injectable({
  providedIn: 'root',
})
export class CharactersService {
  private readonly API_URL = `${environment.apiUrl}/api/characters`;

  constructor(private http: HttpClient) {}

  /**
   * Fetches list of available characters from backend.
   */
  getCharacters(): Observable<CharacterDto[]> {
    return this.http.get<any>(this.API_URL).pipe(
      map((response) => {
        // handle ReferenceHandler.Preserve wrapper: response.$values holds the array
        if (response && response.$values) {
          return response.$values as CharacterDto[];
        }
        // otherwise assume it's already an array
        return response as CharacterDto[];
      }),
      catchError((error) => {
        console.error('Error fetching characters:', error);
        return of([]);
      })
    );
  }
}
