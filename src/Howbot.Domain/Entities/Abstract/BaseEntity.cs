using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Howbot.Domain.Entities.Abstract;
public class BaseEntity
{

  [Key]
  public ulong Id { get; set; }
}
